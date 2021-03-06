import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../models/group';
import { Message } from '../models/message';
import { User } from '../models/User';
import { getPaginatedResult, getPaginationParams } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl
  hubUrl = environment.hubUrl

  private hubConnection: HubConnection  

  private messageThreadSource = new BehaviorSubject<Message[]>([])
  messageThread$ = this.messageThreadSource.asObservable()

  constructor(private http: HttpClient) { }

  createHubConnection(user: User, otherUsername: string){
    console.log(this.hubUrl)
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + "message?user="+otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build()

    this.hubConnection.start().catch(e => console.log(e))

    this.hubConnection.on("ReceiveMessageThread", messages => {
      console.log(messages)
      this.messageThreadSource.next(messages)
    })

    this.hubConnection.on("NewMessage", message => {
      console.log(message)
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        console.log("mess"+messages)
        this.messageThreadSource.next([...messages, message])
      })
    })

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if(group.connections.some(x => x.username === otherUsername)){
        this.messageThread$.pipe(take(1)).subscribe(messages => {
         messages.forEach(message => {
           if(!message.dateRead){
             message.dateRead = new Date(Date.now())
           }
         }) 

         this.messageThreadSource.next([...messages])
        })
      }
    })
  }

  stopHubConnection() {
    if(this.hubConnection) {
      this.hubConnection.stop()
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string){
    let params = getPaginationParams(pageNumber, pageSize)
    params = params.append("Container",container)

    return getPaginatedResult<Message[]>(this.baseUrl+"messages", params, this.http)
  }

  getMessagesThread(username: String){
    return this.http.get<Message[]>(this.baseUrl+'messages/thread/'+username)
  }

  async sendMessage(recipientUsername: string, content: string){
    return this.hubConnection.invoke('SendMessage', {recipientUsername, content})
      .catch(e => console.log(e))
  }

  deleteMessage(id: number){
    return this.http.delete(this.baseUrl+'messages/'+id)
  }
}
