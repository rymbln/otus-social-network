import { Component } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { FORM_TOOLS } from '../shared/form.helper';
import { RegisterReq } from '../model/register.req';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../user.service';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { LoginReq } from '../model/login.req';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  form: FormGroup;
  isReady = false;

  constructor(private srv: UserService,
    private auth: AuthService,
    private router: Router) {
    this.form = new FormGroup({
      first_name: new FormControl('', Validators.required),
      second_name: new FormControl('', Validators.required),
      age: new FormControl(0, Validators.required),
      sex: new FormControl('', Validators.required),
      biography: new FormControl(''),
      city: new FormControl('', Validators.required),
      password: new FormControl('', Validators.required)
    });
    this.isReady = true;
  }

  hasErr(control: AbstractControl, err: string): boolean {   return FORM_TOOLS.hasErr(control, err);  }

  submit(frm: FormGroup) {
    this.form.markAsPristine();
    FORM_TOOLS.markFormGroupTouched(this.form);
    if (!frm.valid) return;
    const data = frm.value as RegisterReq;
    firstValueFrom(this.srv.register(data)).then(res => {
      this.auth.login({id: res.userId, password: data.password} as LoginReq).then(d => this.router.navigate(['/']))
    })
  }
}
