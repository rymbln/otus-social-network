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
  ids: string[] = [
    '3cc744e5-ec95-4926-9a64-aba219819337',
    '3eefe556-a733-4ba9-ba28-3e55cc459a79',
    'b2428a06-ee2a-40b8-94f4-69cd3c85a2e0'
  ];
  id: string = this.ids[0];
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
