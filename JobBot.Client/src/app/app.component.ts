import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Router, RouterModule, RouterOutlet, Routes } from '@angular/router';
import { routes } from './app.routes';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    RouterModule,
    CommonModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  title = 'JobBot.Client';

  routes: Routes = routes;

  constructor(private router: Router) {}

  getClass(path: string) {
    let x = this.router.url;
    if (x.charAt(0) == '/')
      x = x.substring(1);
    if (x == path) {
      return "block py-2 px-3 text-white bg-purple-700 rounded-sm md:bg-transparent md:text-purple-700 md:p-0 dark:text-white md:dark:text-purple-500";
    } else {
      return "block py-2 px-3 text-gray-900 rounded-sm hover:bg-gray-100 md:hover:bg-transparent md:border-0 md:hover:text-purple-700 md:p-0 dark:text-white md:dark:hover:text-purple-500 dark:hover:bg-gray-700 dark:hover:text-white md:dark:hover:bg-transparent";
    }
  }
}

