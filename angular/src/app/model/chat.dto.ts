export interface ChatDTO {
id: string;
name: string;
userId: string;
userName: string;
}

export interface ChatView {
  chatId: string;
  chatName: string;
  userId: string;
  userName: string;
}

export interface ChatMessageView {
  id: string;
  chatId: string;
  userId: string;
  userName: string;
  messageText: string;
  isNew: boolean;
  timestamp: string;
}

export class ChatMessageForm {
  constructor(
    public message: string
  ){}
}


export interface MessageHubModel {
  userId: string;
  chatName: string;
  message: string;
}
