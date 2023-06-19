export interface PostDto {
  id: string;
  authorUserId: string;
  authorUserName: string;
  text: string;
  timeStamp: string;
}

export interface PostView {
  postId: string;
  postText: string;
  timestamp: string;
  friendId: string;
  friendName: string;
}
