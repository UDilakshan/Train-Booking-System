import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
    title: 'Sri Lanka Railway | Colombo Fort - Badulla Reservations',
  },
  {
    path: 'search',
    loadComponent: () => import('./features/search/search.component').then((m) => m.SearchComponent),
    title: 'Search Journeys',
  },
  {
    path: 'booking/:journeyId',
    loadComponent: () => import('./features/booking/booking.component').then((m) => m.BookingComponent),
    title: 'Select Seats',
  },
  {
    path: 'booking/confirmation/:reference',
    loadComponent: () =>
      import('./features/booking-confirmation/booking-confirmation.component').then((m) => m.BookingConfirmationComponent),
    title: 'Booking Confirmed',
  },
  {
    path: 'my-bookings',
    loadComponent: () => import('./features/my-bookings/my-bookings.component').then((m) => m.MyBookingsComponent),
    title: 'My Bookings',
  },
  {
    path: 'admin/login',
    loadComponent: () => import('./features/admin/login/admin-login.component').then((m) => m.AdminLoginComponent),
    title: 'Admin Login',
  },
  {
    path: 'admin/dashboard',
    loadComponent: () => import('./features/admin/dashboard/admin-dashboard.component').then((m) => m.AdminDashboardComponent),
    canActivate: [authGuard],
    title: 'Admin Dashboard',
  },
  { path: '**', redirectTo: '' },
];
