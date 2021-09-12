import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../models/message';
import { getPaginatedResult, getPaginationParams } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl

  constructor(private http: HttpClient) { }

  getMessages(pageNumber: number, pageSize: number, container: string){
    let params = getPaginationParams(pageNumber, pageSize)
    params = params.append("Container",container)

    return getPaginatedResult<Message[]>(this.baseUrl+"messages", params, this.http)
  }

  getMessagesThread(username: String){
    return this.http.get<Message[]>(this.baseUrl+'messages/thread/'+username)
  }

  sendMessage(recipientUsername: string, content: string){
    return this.http.post<Message>(this.baseUrl+'messages', {recipientUsername, content})
  }

  deleteMessage(id: number){
    return this.http.delete(this.baseUrl+'messages/'+id)
  }
}
