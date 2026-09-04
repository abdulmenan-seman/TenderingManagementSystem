import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { BidService } from '../../../core/services/bid.service';

@Component({
  selector: 'app-my-bids',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  template: `
    <section class="space-y-6">
      <header><h1 class="text-2xl font-bold text-slate-800">My Bids</h1><p class="text-sm text-slate-500">Your submitted proposals and documents</p></header>
      @if (bidService.error()) { <p class="rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ bidService.error() }}</p> }
      @if (!bidService.loading() && !bidService.bids().length) { <div class="rounded-2xl border border-slate-200 bg-white p-10 text-center text-sm text-slate-500">You have not submitted any bids.</div> }
      <div class="grid gap-4 lg:grid-cols-2">
        @for (bid of bidService.bids(); track bid.id) {
          <article class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
            <div class="flex items-start justify-between gap-4"><div><h2 class="font-bold text-slate-800">Tender #{{ bid.tenderId }}</h2><p class="text-xs text-slate-500">Submitted {{ bid.submissionDate | date:'mediumDate' }}</p></div><span class="rounded-full bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">{{ bid.status }}</span></div>
            <p class="mt-4 text-lg font-bold text-slate-800">{{ bid.financialProposalAmount | currency:'USD' }}</p>
            <div class="mt-4 border-t border-slate-100 pt-4"><p class="text-xs font-semibold uppercase text-slate-500">Documents ({{ bid.documents.length }})</p><div class="mt-2 space-y-2">@for (doc of bid.documents; track doc.id) { <button type="button" (click)="download(doc.filePath, doc.fileName)" class="block w-full rounded-lg bg-slate-50 px-3 py-2 text-left text-sm text-blue-700 hover:bg-blue-50">{{ doc.fileName }}</button> } @empty { <p class="text-xs text-slate-400">No documents attached.</p> }</div></div>
            @if (bid.status === 'Submitted') { <button type="button" (click)="withdraw(bid.id)" class="mt-4 rounded-lg border border-red-200 px-3 py-2 text-xs font-semibold text-red-600 hover:bg-red-50">Withdraw bid</button> }
          </article>
        }
      </div>
    </section>
  `
})
export class MyBidsComponent implements OnInit {
  readonly bidService = inject(BidService);
  ngOnInit(): void { this.bidService.loadMine().subscribe(); }
  download(path: string, name: string): void { this.bidService.download(path, name); }
  withdraw(id: number): void { if (confirm('Withdraw this bid?')) this.bidService.withdraw(id).subscribe(); }
}