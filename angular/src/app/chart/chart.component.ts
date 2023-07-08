import { Component } from '@angular/core';
import { SignalrService } from '../signalr.service';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { ChartConfiguration, ChartType } from 'chart.js';

@Component({
  selector: 'app-chart',
  templateUrl: './chart.component.html',
  styleUrls: ['./chart.component.scss']
})
export class ChartComponent {

  chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    scales: {
      y: {
        min: 0
      }
    }
  };
  chartLabels: string[] = ['Real time data for the chart'];
  chartType: ChartType = 'bar';
  chartLegend: boolean = true;

  constructor(public signalRService: SignalrService, private http: HttpClient) { }

  ngOnInit() {
    this.signalRService.startConnection();
    this.signalRService.addTransferChartDataListener();
    this.signalRService.addBroadcastChartDataListener();
    this.startHttpRequest();
  }

  private startHttpRequest = () => {
    this.http.get(`${environment.api}/chart`)
      .subscribe(res => {
        console.log(res);
      })
  }

  public chartClicked = (event: any) => {
    console.log(event);
    this.signalRService.broadcastChartData();
  }
}
