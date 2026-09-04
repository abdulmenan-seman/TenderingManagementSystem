import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TenderService } from '../../../../core/services/tender.service';
import { Tender, TenderStatus } from '../../../../core/models/tender.model';

@Component({
  selector: 'app-tender-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './tender-management.html',
  styleUrl: './tender-management.scss'
})
export class TenderManagementComponent implements OnInit {
  public tenderService = inject(TenderService);
  private fb = inject(FormBuilder);

  TenderStatus = TenderStatus;

  // Pagination & Filter Signals
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  selectedStatusFilter = signal<TenderStatus | ''>('');

  // Modal Signals
  isFormModalOpen = signal<boolean>(false);
  isAwardModalOpen = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  editingTenderId = signal<number | null>(null);
  selectedTenderForAward = signal<Tender | null>(null);
  
  // File Staging
  stagedFiles = signal<File[]>([]);

  // Forms
  tenderForm!: FormGroup;
  awardForm!: FormGroup;

  ngOnInit(): void {
    this.initForms();
    this.loadTenders();
  }

  private initForms(): void {
    this.tenderForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      referenceNumber: ['', [Validators.required]],
      description: ['', [Validators.required]],
      estimatedBudget: [0, [Validators.required, Validators.min(1)]],
      submissionDeadline: ['', [Validators.required]]
    });

    this.awardForm = this.fb.group({
      winningBidId: ['', [Validators.required]]
    });
  }

  loadTenders(): void {
    const filter = this.selectedStatusFilter() || undefined;
    this.tenderService.getTenders(this.currentPage(), this.pageSize(), filter).subscribe();
  }

  onFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as TenderStatus | '';
    this.selectedStatusFilter.set(value);
    this.currentPage.set(1);
    this.loadTenders();
  }

  // --- Modal Controllers ---

  openCreateModal(): void {
    this.isEditing.set(false);
    this.editingTenderId.set(null);
    this.tenderForm.reset();
    this.stagedFiles.set([]);
    this.isFormModalOpen.set(true);
  }

  openEditModal(tender: Tender): void {
    this.isEditing.set(true);
    this.editingTenderId.set(tender.id);
    this.tenderForm.patchValue({
      title: tender.title,
      referenceNumber: tender.referenceNumber,
      description: tender.description,
      estimatedBudget: tender.estimatedBudget,
      submissionDeadline: tender.submissionDeadline ? new Date(tender.submissionDeadline).toISOString().split('T')[0] : ''
    });
    this.stagedFiles.set([]);
    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    this.isFormModalOpen.set(false);
  }

  openAwardModal(tender: Tender): void {
    this.selectedTenderForAward.set(tender);
    this.awardForm.reset();
    this.isAwardModalOpen.set(true);
  }

  closeAwardModal(): void {
    this.isAwardModalOpen.set(false);
    this.selectedTenderForAward.set(null);
  }

  // --- File Handling ---
  
  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      const currentFiles = this.stagedFiles();
      this.stagedFiles.set([...currentFiles, ...Array.from(files as FileList)]);
    }
  }

  removeFile(index: number): void {
    const currentFiles = this.stagedFiles();
    currentFiles.splice(index, 1);
    this.stagedFiles.set([...currentFiles]);
  }

  // --- Actions ---

  saveTender(): void {
    if (this.tenderForm.invalid) return;

    const command = this.tenderForm.getRawValue() as any;
    
    // Ensure the date is in the future by setting it to the end of the selected day in UTC
    if (command.submissionDeadline) {
      const date = new Date(command.submissionDeadline);
      date.setUTCHours(23, 59, 59, 999);
      command.submissionDeadline = date.toISOString();
    }

    const files = this.stagedFiles();

    if (this.isEditing() && this.editingTenderId()) {
      this.tenderService.updateTender(this.editingTenderId()!, command).subscribe({
        next: () => {
          this.closeFormModal();
          this.loadTenders();
        },
        error: (err) => {
          console.error("Failed to update tender:", err);
        }
      });
    } else {
      this.tenderService.createTenderWithDocuments(command, files).subscribe({
        next: () => {
          this.closeFormModal();
          this.loadTenders();
        },
        error: (err) => {
          console.error("Failed to save tender:", err);
        }
      });
    }
  }

  publish(id: number): void {
    if (confirm('Are you sure you want to publish this tender to bidders?')) {
      this.tenderService.publishTender(id).subscribe();
    }
  }

  startEvaluation(id: number): void {
    if (confirm('Transition this tender to evaluation phase?')) {
      this.tenderService.startEvaluation(id).subscribe();
    }
  }

  confirmAward(): void {
    if (this.awardForm.invalid || !this.selectedTenderForAward()) return;
    const bidId = this.awardForm.value.winningBidId;

    this.tenderService.awardTender(this.selectedTenderForAward()!.id, bidId).subscribe({
      next: () => {
        this.closeAwardModal();
      }
    });
  }

  closeTender(id: number): void {
    if (confirm('Are you sure you want to close this tender?')) {
      this.tenderService.closeTender(id).subscribe();
    }
  }

  deleteTender(id: number): void {
    if (confirm('Are you sure you want to delete this draft tender?')) {
      this.tenderService.deleteTender(id).subscribe();
    }
  }
}