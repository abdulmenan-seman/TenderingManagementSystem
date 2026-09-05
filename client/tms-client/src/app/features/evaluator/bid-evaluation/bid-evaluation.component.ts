import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-bid-evaluation',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white p-6 rounded-2xl border border-slate-200/80 shadow-sm">
        <h2 class="text-xl font-bold text-slate-800">Bid Scoring & Evaluation</h2>
        <p class="text-xs text-slate-500 font-medium mt-0.5">Evaluate vendor proposal against technical (60%) and financial (40%) criteria</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Proposal Sidebar -->
        <div class="bg-white p-6 rounded-2xl border border-slate-200/80 space-y-4">
          <h3 class="font-bold text-slate-800 text-sm uppercase tracking-wide">Supplier Submission</h3>
          <div class="p-3 bg-slate-50 rounded-xl border border-slate-200 text-xs space-y-1">
            <p class="font-bold text-slate-700">Vendor: TechCorp Solutions Ltd</p>
            <p class="text-slate-500">Bid Amount: $45,000</p>
          </div>
          <div class="space-y-2">
            <span class="text-xs font-bold text-slate-600 block">Submitted Files</span>
            <div class="p-2.5 bg-blue-50/50 rounded-xl border border-blue-100 flex items-center justify-between text-xs">
              <span class="truncate font-medium text-slate-700">Technical_Proposal.pdf</span>
              <mat-icon class="text-blue-600 !w-4 !h-4 !text-base cursor-pointer">download</mat-icon>
            </div>
            <div class="p-2.5 bg-blue-50/50 rounded-xl border border-blue-100 flex items-center justify-between text-xs">
              <span class="truncate font-medium text-slate-700">Financial_Breakdown.xlsx</span>
              <mat-icon class="text-blue-600 !w-4 !h-4 !text-base cursor-pointer">download</mat-icon>
            </div>
          </div>
        </div>

        <!-- Evaluation Form -->
        <div class="lg:col-span-2 bg-white p-6 rounded-2xl border border-slate-200/80 space-y-6">
          <h3 class="font-bold text-slate-800 text-sm uppercase tracking-wide">Scoring Criteria</h3>
          
          <div class="space-y-4">
            <div>
              <div class="flex justify-between text-xs font-bold text-slate-700 mb-1">
                <span>Technical Compliance & Infrastructure (Max 60 Points)</span>
                <span class="text-blue-700">{{ techScore() }} / 60</span>
              </div>
              <input type="range" min="0" max="60" [(ngModel)]="techScore" class="w-full accent-[#1b4380]">
            </div>

            <div>
              <div class="flex justify-between text-xs font-bold text-slate-700 mb-1">
                <span>Financial Viability & Cost Efficiency (Max 40 Points)</span>
                <span class="text-blue-700">{{ financialScore() }} / 40</span>
              </div>
              <input type="range" min="0" max="40" [(ngModel)]="financialScore" class="w-full accent-[#1b4380]">
            </div>

            <div>
              <label class="block text-xs font-bold text-slate-700 uppercase mb-2">Evaluator Comments & Justification</label>
              <textarea rows="3" placeholder="Enter evaluation remarks..." class="w-full p-3 text-xs border border-slate-200 rounded-xl focus:outline-none focus:ring-1 focus:ring-blue-600"></textarea>
            </div>
          </div>

          <div class="pt-4 border-t border-slate-100 flex items-center justify-between">
            <span class="text-sm font-bold text-slate-800">Total Score: <span class="text-blue-700 text-lg">{{ techScore() + financialScore() }} / 100</span></span>
            <button class="px-5 py-2.5 bg-[#1b4380] text-white text-xs font-semibold rounded-xl hover:bg-blue-900 transition-colors">
              Submit Evaluation Score
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class BidEvaluationComponent {
  techScore = signal<number>(45);
  financialScore = signal<number>(30);
}