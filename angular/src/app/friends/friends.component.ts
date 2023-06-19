import { Component } from '@angular/core';
import { BehaviorSubject, Observable, of, shareReplay, switchMap, tap } from 'rxjs';
import { FriendDto } from '../model/friend.model';
import { FriendService } from '../friend.service';
import { ConfirmationService } from 'primeng/api';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';

@UntilDestroy()
@Component({
  selector: 'app-friends',
  templateUrl: './friends.component.html',
  styleUrls: ['./friends.component.scss']
})
export class FriendsComponent {
  private _refresh = new BehaviorSubject<boolean>(false);
  private _search = new BehaviorSubject<string>('');

  friends$: Observable<FriendDto[]> = this._refresh.asObservable().pipe(
    switchMap(e => this.srv.getFriends()),
    tap(data => console.log(data)),
    shareReplay()
  );

  searchResults$: Observable<FriendDto[]> = this._search.asObservable().pipe(
    switchMap(e => e === '' ? of([]) : this.srv.searchFriend(e)),
    tap(data => console.log(data)),
    shareReplay()
  );

  query = '';

  constructor(private srv: FriendService, private confirm: ConfirmationService) { }

  onDelete(friend: FriendDto) {
    this.confirm.confirm({
      message: `Are you sure you want to delete friend ${friend.fullName}?`,
      header: `Confirm delete`,
      icon: 'pi pi-info-circle',
      accept: () => {
        this.srv.deleteFriend(friend.id).pipe(
          untilDestroyed(this),
          tap(e => this._refresh.next(true))
        ).subscribe()
      },
    });
  }

  onAdd(friend: FriendDto) {
    this.confirm.confirm({
      message: `Are you sure you want to add friend ${friend.fullName}?`,
      header: `Confirm adding friend`,
      icon: 'pi pi-info-circle',
      accept: () => {
        this.srv.addFriend(friend.id).pipe(
          untilDestroyed(this),
          tap(e => {
            this._refresh.next(true);
            this.query = '';
            this._search.next('');
          })
        ).subscribe()
      },
    });
  }

  onSearch(val: string) {
    this._search.next(val);
  }
}
