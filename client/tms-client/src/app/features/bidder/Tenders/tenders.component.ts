import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TenderService } from '../../../core/services/tender.service';
import { TenderStatus } from '../../../core/models/tender.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-tenders',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ],
  templateUrl: './tenders.component.html'
})
export class TendersComponent implements OnInit {
  public tenderService = inject(TenderService);

  pageNumber = signal<number>(1);
  pageSize = signal<number>(9);

  ngOnInit(): void {
    this.loadPublishedTenders();
  }

  loadPublishedTenders(): void {
    this.tenderService.getTenders(
      this.pageNumber(),
      this.pageSize(),
      TenderStatus.Published
    ).subscribe({
      error: (err) => console.error('Failed to load published tenders', err)
    });
  }

  changePage(newPage: number): void {
    this.pageNumber.set(newPage);
    this.loadPublishedTenders();
  }

  getDocumentUrl(filePath: string): string {
    if (!filePath) return '#';
    if (filePath.startsWith('http://') || filePath.startsWith('https://')) {
      return filePath;
    }
    const baseUrl = environment.apiUrl?.replace('/api/v1', '') || 'http://localhost:5293';
    return `${baseUrl}/${filePath.replace(/^\/+/, '')}`;
  }

  downloadDocument(event: Event, filePath: string, fileName: string): void {
    event.preventDefault();
    this.tenderService.downloadTenderDocument(filePath, fileName);
  }
}