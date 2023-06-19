import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { UserDto } from './model/user.model';
import { Observable } from 'rxjs';
import { RegisterReq } from './model/register.req';
import { RegisterRes } from './model/register.res';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor( private _http: HttpClient) { }

  getProfile(): Observable<UserDto> {
    return this._http.get<UserDto>(`${environment.api}/user/profile`);
  }

  register(obj: RegisterReq) {
    return this._http.post<RegisterRes>(`${environment.api}/user/register`, obj);
  }
}
