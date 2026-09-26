import { Routes } from '@angular/router';
import { WorkspaceComponent } from './workspace.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: ':view', component: WorkspaceComponent },
  { path: '**', redirectTo: 'dashboard' }
];
