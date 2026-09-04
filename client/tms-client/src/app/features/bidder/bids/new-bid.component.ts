import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BidService } from '../../../core/services/bid.service';

@Component({
  selector: 'app-new-bid',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="max-w-2xl space-y-6">
      <header>
        <p class="text-xs font-semibold uppercase tracking-wider text-blue-600">Bid submission</p>
        <h1 class="mt-1 text-2xl font-bold text-slate-800">Submit your bid</h1>
        <p class="mt-1 text-sm text-slate-500">Tender #{{ tenderId }}</p>
      </header>
      <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-5 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <label class="block text-sm font-semibold text-slate-700">Financial proposal amount
          <input formControlName="amount" type="number" min="0.01" step="0.01" class="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2" />
        </label>
        <div>
          <label class="block text-sm font-semibold text-slate-700">Supporting documents</label>
          <input type="file" multiple (change)="selectFiles($event)" class="mt-2 block w-full text-sm" />
          @if (files().length) { <p class="mt-2 text-xs text-slate-500">{{ files().length }} document(s) selected</p> }
        </div>
        @if (bidService.error()) { <p class="rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ bidService.error() }}</p> }
        <button type="submit" [disabled]="form.invalid || bidService.loading()" class="rounded-lg bg-[#1b4380] px-5 py-2.5 text-sm font-semibold text-white disabled:opacity-50">{{ bidService.loading() ? 'Submitting...' : 'Submit bid' }}</button>
      </form>
    </section>
  `
})
export class NewBidComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  readonly bidService = inject(BidService);
  readonly tenderId = Number(this.route.snapshot.queryParamMap.get('tenderId'));
  readonly files = signal<File[]>([]);
  readonly form = this.fb.nonNullable.group({ amount: [0, [Validators.required, Validators.min(0.01)]] });

  selectFiles(event: Event): void {
    const selected = (event.target as HTMLInputElement).files;
    if (selected) this.files.set(Array.from(selected));
  }

  submit(): void {
    if (this.form.invalid || !this.tenderId) return;
    this.bidService.submitBid(this.tenderId, this.form.getRawValue().amount, this.files()).subscribe({
      next: () => this.router.navigate(['/dashboard/bidder/my-bids'])
    });
  }
}