import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface AssignedTender {
  id: number;
  title: string;
  status: string;
  criteria: Criterion[];
}

interface Criterion {
  id: number;
  criteriaName: string;
  description: string;
  weightPercentage: number;
  maxScore: number;
}

@Component({
  selector: 'app-assigned-tenders',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatButtonModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-sm flex items-center justify-between">
        <div>
          <h2 class="text-xl font-bold text-slate-800">Assigned Tenders for Evaluation</h2>
          <p class="text-xs text-slate-500 font-medium">Review bids submitted for closed public tenders</p>
        </div>
        <span class="px-3 py-1 bg-blue-50 text-blue-700 text-xs font-bold rounded-full border border-blue-100">
          {{ tenders().length }} Assigned
        </span>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        @for (tender of tenders(); track tender.id) {
        <div class="bg-white rounded-2xl border border-slate-200/80 p-6 flex flex-col justify-between shadow-sm hover:shadow-md transition-shadow">
          <div class="space-y-3">
            <div class="flex justify-between items-center">
              <span class="text-xs font-mono font-bold text-slate-500 bg-slate-100 px-2.5 py-1 rounded-md">Tender #{{ tender.id }}</span>
              <span class="text-xs font-bold px-2.5 py-1 rounded-full bg-amber-100 text-amber-800">Evaluation Open</span>
            </div>
            <h3 class="font-bold text-slate-800 text-base">{{ tender.title }}</h3>
            <div class="pt-2 border-t border-slate-100">
              <p class="text-xs font-bold uppercase tracking-wide text-slate-500">Evaluation criteria</p>
              <div class="mt-2 space-y-2">
                @for (criterion of tender.criteria; track criterion.id) {
                  <div class="rounded-lg bg-slate-50 p-2.5">
                    <div class="flex justify-between gap-2 text-xs"><span class="font-semibold text-slate-700">{{ criterion.criteriaName }}</span><span class="font-bold text-blue-700">{{ criterion.weightPercentage }}%</span></div>
                    <p class="mt-1 text-[11px] text-slate-500">{{ criterion.description }} · Max {{ criterion.maxScore }}</p>
                  </div>
                } @empty {
                  <p class="text-xs text-slate-400">No criteria defined.</p>
                }
              </div>
            </div>
          </div>
          <button 
            routerLink="/dashboard/evaluator/bid-evaluation" 
            [queryParams]="{ tenderId: tender.id }"
            class="mt-6 w-full py-2.5 bg-[#1b4380] text-white text-xs font-semibold rounded-xl hover:bg-blue-900 transition-colors flex items-center justify-center gap-2">
            <mat-icon class="!w-4 !h-4 !text-base">fact_check</mat-icon> Evaluate Submissions
          </button>
        </div>
        } @empty {
          <div class="md:col-span-2 lg:col-span-3 rounded-2xl border border-dashed border-slate-300 p-12 text-center text-sm text-slate-500">No tenders have been assigned to you.</div>
        }
      </div>
    </div>
  `
})
export class AssignedTendersComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;
  readonly tenders = signal<AssignedTender[]>([]);

  ngOnInit(): void {
    this.http.get<{ id: number; title: string; status: string }[]>(`${this.apiUrl}/tenders/assigned-to-me`).subscribe(tenders => {
      this.tenders.set(tenders.map(tender => ({ ...tender, criteria: [] })));
      tenders.forEach(tender => this.http.get<Criterion[]>(`${this.apiUrl}/tenders/${tender.id}/evaluation-criteria`).subscribe(criteria => {
        this.tenders.update(items => items.map(item => item.id === tender.id ? { ...item, criteria } : item));
      }));
    });
  }
}