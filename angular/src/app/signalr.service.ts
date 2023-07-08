import { Injectable } from '@angular/core';
import { ChartModel } from './model/chart.model';
import * as signalR from "@microsoft/signalr"
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { HttpTransportType } from '@microsoft/signalr';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  public data: ChartModel[] = [];
  public bradcastedData: ChartModel[] = [];
  private hubConnection!: signalR.HubConnection;
  public connectionId: string = '';

  constructor(private http: HttpClient ,private auth: AuthService) {}

    public startConnection = () => {
      this.hubConnection = new signalR.HubConnectionBuilder()
                              .withUrl(`${environment.websocket}/chart`, {
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
          this.http.get(`${environment.websocket}/chart`)
          .subscribe(res => {
            console.log(res);
          })
        })
    }

    private getConnectionId = () => {
      this.hubConnection.invoke('getconnectionid')
      .then((data) => {
        console.log(data);
        this.connectionId = data;
      });
    }

    public addTransferChartDataListener = () => {
      this.hubConnection.on('transferchartdata', (data) => {
        this.data = data;
        console.log(data);
      });
    }

    public broadcastChartData = () => {
      const data = this.data.map(m => {
        const temp = {
          data: m.data,
          label: m.label
        }
        return temp;
      });
      this.hubConnection.invoke('broadcastchartdata', data, this.connectionId)
      .catch(err => console.error(err));
    }

    public addBroadcastChartDataListener = () => {
      this.hubConnection.on('broadcastchartdata', (data) => {
        this.bradcastedData = data;
      })
    }


}
