import { Component } from '@angular/core';
import { HeroComponent } from './components/hero/hero';
import { FeaturedTendersComponent } from './components/featured-tenders/featured-tenders';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [HeroComponent, FeaturedTendersComponent],
  template: `
    <app-hero></app-hero>
    <app-featured-tenders></app-featured-tenders>
  `
})
export class HomeComponent {}