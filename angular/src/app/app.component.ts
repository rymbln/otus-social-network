import { Component, OnInit } from '@angular/core';
import { MenuItem, MessageService } from 'primeng/api';
import { AuthService } from './auth.service';
import { Observable, map, shareReplay, tap } from 'rxjs';
import { PostHotificationService } from './post-hotification.service';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { ChatNotificationService } from './chat-notification.service';

@UntilDestroy()
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent  implements OnInit {
  title = 'angular';
  items$: Observable<MenuItem[]> = this.auth.isAuth$.pipe(
    map((val: boolean) => {
      const items: MenuItem[] = [];

      if (val) {
        items.push({
          label: 'Me: ' + this.auth.currentUser,
          icon: 'pi pi-id-card',
          routerLink: ['/']
        });
        items.push({
          label: 'Feed',
          icon: 'pi pi-list',
          routerLink: ['/feed']
        });
        items.push({
          label: 'Posts',
          icon: 'pi pi-file',
          routerLink: ['/posts']
        });
        items.push({
          label: 'Friends',
          icon: 'pi pi-users',
          routerLink: ['/friends']
        });
        items.push({
          label: 'Chats',
          icon: 'pi pi-comments',
          routerLink: ['/chats']
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
    private auth: AuthService,
    private signal: PostHotificationService,
    private signalChat: ChatNotificationService,
    private message: MessageService,
    private http: HttpClient
  ) {
    this.auth.isAuth$.pipe(
      untilDestroyed(this),
      tap((auth: boolean) => {
        console.log(auth);
        if (auth) {
          this.signal.startConnection();
          this.signal.addPostNotificationListener();
          this.signalChat.startConnection();
          this.signalChat.addChatNotificationListener();
        }
      }),
      shareReplay()).subscribe();
  }

  ngOnInit(): void {

    // this.startHttpRequest();
  }

  logout() {
    this.auth.logout();
  }

  toast() {
    this.message.add({ severity: 'success', summary: 'Success', detail: 'New Post!' })
  }

  private startHttpRequest = () => {
    this.http.get(`${environment.api}/post/feed/posted`)
      .subscribe(res => {
        this.message.add({ severity: 'success', summary: 'Success', detail: 'New Post!' })
        console.log(res);
      })
  }
}
