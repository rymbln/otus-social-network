import { Injectable } from '@angular/core';
import { PostHubModel } from './model/post.model';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import * as signalR from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { HttpTransportType } from '@microsoft/signalr';
import { MessageService } from 'primeng/api';
import { BehaviorSubject, Observable, shareReplay } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PostHotificationService {

  public data: PostHubModel | undefined;

  private _feedUpdated = new BehaviorSubject<boolean>(false);
  feedUpdated$: Observable<boolean> = this._feedUpdated.asObservable().pipe(shareReplay());

  private hubConnection!: signalR.HubConnection;
  public connectionId: string = '';

  constructor(private http: HttpClient, private auth: AuthService, private msg: MessageService) {

  }

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.websocket}/feed/news`, {
        accessTokenFactory: () => this.auth.jwt,
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,

      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .then(() => this.getConnectionId())
      .catch(err => console.log('Error while starting connection: ' + err));

    this.hubConnection.onreconnected(() => {
      this.http.get(`${environment.websocket}/feed/news?access_token=${this.auth.jwt}`)
        .subscribe(res => {
          console.log(res);
        })
    })
  }

  private getConnectionId() {
    this.hubConnection.invoke('getconnectionid')
    .then((data) => {
      console.log(data);
      this.connectionId = data;
    });
  }

  addPostNotificationListener() {
    this.hubConnection.on('posted', (data: PostHubModel) => {
      this.data = data;
      this._feedUpdated.next(true);
      this.msg.add({ severity: 'info', summary: `New Post from ${data.authorUserId}!`, detail: data.postText })
      console.log(data);
    })
  }

  sendPostNotification(data: PostHubModel) {
    this.hubConnection.invoke('posted', data)
    .catch(err => console.error(err));
  }
}
