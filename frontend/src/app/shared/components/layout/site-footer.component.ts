import { Component } from '@angular/core';

@Component({
  selector: 'app-site-footer',
  standalone: true,
  template: `
    <footer class="footer">
      <span>&copy; {{ year }} Sri Lanka Railway Department — Reservation System</span>
      <span>Colombo Fort &rarr; Badulla Upcountry Line</span>
    </footer>
  `,
  styles: [
    `
      .footer {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        gap: 0.5rem;
        padding: 1.5rem;
        margin-top: 3rem;
        border-top: 1px solid rgba(0, 0, 0, 0.12);
        color: rgba(0, 0, 0, 0.6);
        font-size: 0.85rem;
      }
    `,
  ],
})
export class SiteFooterComponent {
  protected readonly year = new Date().getFullYear();
}
