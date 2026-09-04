import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, switchMap, tap, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Bid } from '../models/bid.model';

@Injectable({ providedIn: 'root' })
export class BidService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/bids`;
  readonly bids = signal<Bid[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  submitBid(tenderId: number, amount: number, files: File[]): Observable<Bid> {
    this.loading.set(true);
    this.error.set(null);
    return this.http.post<{ bidId: number }>(this.apiUrl, {
      tenderId,
      supplierId: 0,
      financialProposalAmount: amount
    }).pipe(
      switchMap(({ bidId }) => files.length
        ? forkJoin(files.map(file => this.uploadDocument(bidId, file))).pipe(
          switchMap(() => this.findMineBid(bidId)))
        : this.http.get<Bid[]>(`${this.apiUrl}/mine`).pipe(
          switchMap(bids => of(bids.find(bid => bid.id === bidId)!)))),
      tap({
        next: () => this.loading.set(false),
        error: (err) => {
          this.loading.set(false);
          this.error.set(err?.error?.detail || 'Failed to submit bid.');
        }
      })
    );
  }

  private findMineBid(bidId: number): Observable<Bid> {
    return this.http.get<Bid[]>(`${this.apiUrl}/mine`).pipe(
      switchMap(bids => {
        const bid = bids.find(item => item.id === bidId);
        return bid ? of(bid) : of({} as Bid);
      })
    );
  }

  uploadDocument(bidId: number, file: File, documentType = 'SupportingDocument'): Observable<unknown> {
    const form = new FormData();
    form.append('documentType', documentType);
    form.append('file', file, file.name);
    return this.http.post(`${this.apiUrl}/${bidId}/documents`, form);
  }

  loadMine(): Observable<Bid[]> {
    this.loading.set(true);
    return this.http.get<Bid[]>(`${this.apiUrl}/mine`).pipe(tap({
      next: bids => { this.bids.set(bids); this.loading.set(false); },
      error: err => { this.error.set(err?.error?.detail || 'Failed to load bids.'); this.loading.set(false); }
    }));
  }

  withdraw(bidId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${bidId}`).pipe(
      tap(() => this.bids.update(bids => bids.filter(bid => bid.id !== bidId)))
    );
  }

  download(documentPath: string, fileName: string): void {
    const url = `${environment.apiUrl.replace('/api/v1', '')}${documentPath}`;
    this.http.get(url, { responseType: 'blob' }).subscribe(blob => {
      const objectUrl = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = objectUrl;
      anchor.download = fileName;
      anchor.click();
      URL.revokeObjectURL(objectUrl);
    });
  }
}