import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { FriendDto } from './model/friend.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FriendService {

  constructor( private _http: HttpClient) { }

  getFriends(): Observable<FriendDto[]> {
    return this._http.get<FriendDto[]>(`${environment.api}/friend`);
  }

  addFriend(userId: string) : Observable<string> {
    return this._http.put<string>(`${environment.api}/friend/set/${userId}`, {});
  }

  deleteFriend(userId: string) : Observable<string> {
    return this._http.put<string>(`${environment.api}/friend/delete/${userId}`, {});
  }

  searchFriend(q: string): Observable<FriendDto[]> {
    return this._http.get<FriendDto[]>(`${environment.api}/friend/search/${q}`);
  }
}
