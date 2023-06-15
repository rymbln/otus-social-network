import { Component } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { AuthService } from './auth.service';
import { Observable, map, shareReplay } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'angular';
  isAuth$ = this.auth.isAuth$.pipe(shareReplay());
  items$: Observable<MenuItem[]> = this.auth.isAuth$.pipe(
    map((val: boolean) => {
      const items: MenuItem[] = [];

      if (val) {
        items.push({
          label: 'Me',
          icon: 'pi pi-id-card',
          routerLink: ['/']
        });
        items.push({
          label: 'Logout',
          icon: 'pi pi-sign-out',
          command: () => {
            this.logout();
          }
        });
      } else {
        items.push({
          label: 'Login',
          icon: 'pi pi-sign-in',
          routerLink: ['/login']
        });
        items.push({
          label: 'Register',
          icon: 'pi pi-sign-in',
          routerLink: ['/register']
        });
      }
      return items;
    }),
    shareReplay()
  )

  // [
  //   {
  //     label: 'Login',
  //     icon: 'pi pi-sign-in',
  //     routerLink: ['/login']
  //   },

  // ];

  constructor(
    private auth: AuthService
  ) {

  }

  logout() { }
}
