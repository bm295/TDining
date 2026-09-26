import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({ selector: 'td-root', standalone: true, imports: [RouterOutlet], template: '<router-outlet />' })
export class AppComponent {}
