import { Component } from '@angular/core';
import { HeroComponent } from './components/hero/hero';
import { FeaturedTendersComponent } from './components/featured-tenders/featured-tenders';
import { AboutComponent } from './components/about/about';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [HeroComponent, FeaturedTendersComponent, AboutComponent],
  template: `
    <app-hero></app-hero>
    <app-featured-tenders></app-featured-tenders>
    <app-about></app-about>
  `
})
export class HomeComponent {}