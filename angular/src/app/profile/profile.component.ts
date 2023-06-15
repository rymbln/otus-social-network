import { Component } from '@angular/core';
import { AuthService } from '../auth.service';
import { shareReplay } from 'rxjs';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent {
isAuth$ = this.auth.isAuth$.pipe(shareReplay());
constructor(private auth: AuthService){}
}
