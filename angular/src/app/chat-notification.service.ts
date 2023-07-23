import { Injectable } from "@angular/core";
import { HttpTransportType } from "@microsoft/signalr";
import { MessageService } from "primeng/api";
import { BehaviorSubject, Observable, shareReplay } from "rxjs";
import { AuthService } from "./auth.service";
import * as signalR from "@microsoft/signalr";
import { environment } from "src/environments/environment";
import { MessageHubModel } from "./model/chat.dto";
import { HttpClient } from "@angular/common/http";

@Injectable({
  providedIn: 'root'
})
export class ChatNotificationService {

  public data: MessageHubModel | undefined;

  private _feedUpdated = new BehaviorSubject<boolean>(false);
  feedUpdated$: Observable<boolean> = this._feedUpdated.asObservable().pipe(shareReplay());

  private hubConnection!: signalR.HubConnection;
  public connectionId: string = '';

  constructor(private http: HttpClient, private auth: AuthService, private msg: MessageService) {

  }

  private getConnectionId() {
    this.hubConnection.invoke('getconnectionid')
    .then((data) => {
      console.log(data);
      this.connectionId = data;
    });
  }

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.websocket}/feed/chat`, {
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
      this.http.get(`${environment.websocket}/feed/chat?access_token=${this.auth.jwt}`)
        .subscribe(res => {
          console.log(res);
        })
    })
  }

  addChatNotificationListener() {
    this.hubConnection.on('received', (data: MessageHubModel) => {
      this.data = data;
      this._feedUpdated.next(true);
      this.msg.add({ severity: 'info', summary: `New Message in ${data.chatName}!`, detail: data.message })
      console.log(data);
    })
  }

  sendMessage(data: MessageHubModel) {
    this.hubConnection.invoke('sended', data)
    .catch(err => console.error(err));
  }
}
