import { Component } from '@angular/core';
import { AuthService } from '../auth.service';
import { shareReplay } from 'rxjs';
import { UserService } from '../user.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent {
  isAuth$ = this.auth.isAuth$.pipe(shareReplay());
  user$ = this.userSrv.getProfile();
  constructor(private auth: AuthService, private userSrv: UserService) { }
}
