import { Component } from '@angular/core';
import { AuthService } from '../auth.service';
import { LoginReq } from '../model/login.req';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  id: string = '3cc744e5-ec95-4926-9a64-aba219819337';
  password: string = 'qwerty';

  constructor(private _auth: AuthService, private router: Router) { }

  async login() {
    console.log('login');
    const form = { id: this.id, password: this.password } as LoginReq;
    const res = await this._auth.login(form);
    if (res) {
      this.router.navigate(['/']);
    }
}
}
