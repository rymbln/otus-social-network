import { Component } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest, map, of, shareReplay, switchMap, tap } from 'rxjs';
import { ChatsService } from '../chats.service';
import { ChatDTO, ChatMessageForm, ChatMessageView, ChatView } from '../model/chat.dto';
import { FriendDto } from '../model/friend.model';
import { FriendService } from '../friend.service';
import { SelectItem } from 'primeng/api';
import { CreateChatReq } from '../model/create-chat.req';
import { AuthService } from '../auth.service';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';

@UntilDestroy()
@Component({
  selector: 'app-chats',
  templateUrl: './chats.component.html',
  styleUrls: ['./chats.component.scss']
})
export class ChatsComponent {

  isShowDialog = false;
  message: string = '';

  private _refresh = new BehaviorSubject<boolean>(false);
  private _selectedChat = new BehaviorSubject<ChatView | undefined>(undefined);

  get selectedChat() {
    return this._selectedChat.value;
  }

  chats$: Observable<ChatView[]> = this._refresh.asObservable().pipe(
    switchMap(e => this._srv.getAll()),
    tap(data => {
      this.friendsFromChat = data.map(o => o.userId);
    }),
    shareReplay()
  );

  messages$: Observable<ChatMessageView[]> = combineLatest([
    this._selectedChat.asObservable(),
    this._refresh.asObservable()
  ]).pipe(
      map(data => data[0]),
      switchMap(chat => {
        if (chat) {
          return this._srv.getMessages(chat?.chatId)
        } else {
          return of([])
        }
      }),
      shareReplay()
  )



  friends$: Observable<SelectItem[]> = combineLatest([
    this._refresh.asObservable(),
    this.chats$
  ]).pipe(
    switchMap(e => this._srvFriends.getFriends()),
    map(data => data.filter(o => this.friendsFromChat.indexOf(o.id) == -1).map(o => ({ label: o.fullName, value: o.id } as SelectItem))),
    shareReplay()
  );
  private friendsFromChat: string[] = [];

  selectedFriend: string | undefined;

  constructor(
    private _srv: ChatsService,
    private _srvFriends: FriendService,
    private auth: AuthService,
  ) {
    this._refresh.next(true);
  }

  onNew(chats: ChatView[]) {
    this.friendsFromChat = chats.map(o => o.userId);
    this.isShowDialog = true;
    this.selectedFriend = undefined;
  }

  onCreateChat(userId: string) {
    const form = new CreateChatReq(userId);
    console.log(form);
    this._srv.create(form).pipe(
      untilDestroyed(this),
      tap(data =>  {
        this._refresh.next(true);
        this.isShowDialog = false;
      })
    ).subscribe();
  }
  onRefresh() {
    this._refresh.next(true);
  }
  onSelectChat(chat: ChatView) {
    this._selectedChat.next(chat);
  }
  onSend() {
    const form = new ChatMessageForm(this.message);
    this._srv.sendMessages(this._selectedChat.value?.chatId ?? '', form).pipe(
      untilDestroyed(this),
      tap(data => {
        this.message = '';
        this._refresh.next(true);
      })
    ).subscribe();
  }
}
