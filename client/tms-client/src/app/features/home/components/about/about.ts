import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    MatButtonModule, 
    MatIconModule
  ],
  templateUrl: './about.html',
  styleUrl: './about.scss'
})
export class AboutComponent {
  pillars = [
    {
      icon: 'visibility',
      title: 'Transparency',
      description: 'Public audit logs and unalterable bidding histories ensure unbiased evaluations.'
    },
    {
      icon: 'gavel',
      title: 'Regulatory Compliance',
      description: 'Built-in adherence rules aligned strictly with public procurement laws.'
    },
    {
      icon: 'bolt',
      title: 'High Efficiency',
      description: 'Automated evaluation pipelines reduce approval timelines from weeks to hours.'
    },
    {
      icon: 'shield',
      title: 'Data Integrity',
      description: 'Bank-grade encryption safeguards tender documents and financial bids.'
    }
  ];

  stats = [
    { value: '$1.2B+', label: 'Tenders Processed' },
    { value: '4,500+', label: 'Verified Suppliers' },
    { value: '99.8%', label: 'Compliance Rate' },
    { value: '24/7', label: 'Automated Evaluation' }
  ];
}