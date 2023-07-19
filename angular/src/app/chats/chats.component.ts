import { Component, ViewChild } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest, map, of, shareReplay, switchMap, tap } from 'rxjs';
import { ChatsService } from '../chats.service';
import { ChatDTO, ChatMessageForm, ChatMessageView, ChatView } from '../model/chat.dto';
import { FriendDto } from '../model/friend.model';
import { FriendService } from '../friend.service';
import { ConfirmationService, SelectItem } from 'primeng/api';
import { CreateChatReq } from '../model/create-chat.req';
import { AuthService } from '../auth.service';
import { UntilDestroy, untilDestroyed } from '@ngneat/until-destroy';
import { ScrollPanel } from 'primeng/scrollpanel';

@UntilDestroy()
@Component({
  selector: 'app-chats',
  templateUrl: './chats.component.html',
  styleUrls: ['./chats.component.scss']
})
export class ChatsComponent {
@ViewChild("msgScroll") msgScroll: ScrollPanel | undefined;

  isShowDialog = false;
  message: string = '';

  private _refreshChat = new BehaviorSubject<boolean>(false);
  private _refreshMessage = new BehaviorSubject<boolean>(false);
  private _selectedChat = new BehaviorSubject<ChatView | undefined>(undefined);

  get selectedChat() {
    return this._selectedChat.value;
  }

  chats$: Observable<ChatView[]> = this._refreshChat.asObservable().pipe(
    switchMap(e => this._srv.getAll()),
    tap(data => {
      this.friendsFromChat = data.map(o => o.userId);
    }),
    shareReplay()
  );

  messages$: Observable<ChatMessageView[]> = combineLatest([
    this._selectedChat.asObservable(),
    this._refreshMessage.asObservable(),
    this._selectedChat.asObservable()
  ]).pipe(
      map(data => data[0]),
      tap(data => console.log(data)),
      switchMap(chat => {

        if (chat) {
          return this._srv.getMessages(chat?.chatId)
        } else {
          return of([])
        }
      }),
      tap(data => { console.log(data);
        this.msgScroll?.scrollTop(10000);
      }),
      shareReplay()
  )



  friends$: Observable<SelectItem[]> = combineLatest([
    this._refreshChat.asObservable(),
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
    private confirm: ConfirmationService,
  ) {
    this._refreshChat.next(true);
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
        this._refreshChat.next(true);
        this.isShowDialog = false;
      })
    ).subscribe();
  }
  onRefresh() {
    this._refreshChat.next(true);
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
        this._refreshMessage.next(true);

      })
    ).subscribe();
  }

  onDeleteMessage(chatId: string, messageId: string) {
    this.confirm.confirm({
      message: `Are you sure you want to delete message?`,
      header: `Confirm delete`,
      icon: 'pi pi-info-circle',
      accept: () => {
        this._srv.deleteMessage(chatId, messageId).pipe(
          untilDestroyed(this),
          tap(e => this._refreshMessage.next(true))
        ).subscribe()
      },
    });
  }

  onDeleteChat(chatId: string) {
    this.confirm.confirm({
      message: `Are you sure you want to delete chat?`,
      header: `Confirm delete`,
      icon: 'pi pi-info-circle',
      accept: () => {
        this._srv.deleteChat(chatId).pipe(
          untilDestroyed(this),
          tap(e => {
            this._selectedChat.next(undefined);
            this._refreshChat.next(true);
          })
        ).subscribe()
      },
    });
  }
}
