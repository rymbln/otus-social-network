import { Injectable } from '@angular/core';

import { CreateChatReq } from './model/create-chat.req';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { ChatDTO, ChatMessageForm, ChatMessageView, ChatView } from './model/chat.dto';

@Injectable({
  providedIn: 'root'
})
export class ChatsService {

  constructor( private _http: HttpClient) { }

  create(obj: CreateChatReq) {
    return this._http.post(`${environment.api}/chat`, obj);
  }
  delete(id: string) {
    return this._http.delete(`${environment.api}/chat/${id}`);
  }

  getAll() {
    return this._http.get<ChatView[]>(`${environment.api}/chat`);
  }
  getMessages(id: string) {
    return this._http.get<ChatMessageView[]>(`${environment.api}/chat/${id}/messages`);
  }
  sendMessages(id: string, form: ChatMessageForm) {
    return this._http.post<string>(`${environment.api}/chat/${id}/messages`, form);
  }
  deleteMessage(chatId: string, messageId: string) {
    return this._http.delete(`${environment.api}/chat/${chatId}/messages/${messageId}`);
  }
  deleteChat(chatId: string) {
    return this._http.delete(`${environment.api}/chat/${chatId}`);
  }
}
