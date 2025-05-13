import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Pagination, PaginationedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { Group } from '../_models/group';
import { BusyService } from './busy.service';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubsUrl;
  private http = inject(HttpClient);
  private busyService = inject(BusyService);
  hubConnection? : HubConnection
  paginatedResult = signal<PaginationedResult<Message[]>|null>(null)
  messageThread = signal<Message[]>([])

 createHubConnection(user : User , otherUsername : string) {
  this.busyService.busy();
   this.hubConnection = new HubConnectionBuilder()
       .withUrl(this.hubUrl + 'message?user=' + otherUsername,{
        accessTokenFactory: () => user.token
    })
       .withAutomaticReconnect()
       .build();

       this.hubConnection.start().catch(error => console.log(error) )
       .finally(()=> this.busyService.idle());

       this.hubConnection.on('Receive Message Thread' , messages => {
        this.messageThread.set(messages);
   })
      this.hubConnection.on('New Message' , message => {
    this.messageThread.update(messages => [...messages , message])
   })

    this.hubConnection.on('Updated Group' , (group: Group)=>{
      
    if (group.connections.some(x => x.userName === otherUsername)) {
      this.messageThread.update(messages => {
         messages.forEach (message => {
           if (!message.dateRead)
           {
            message.dateRead = new Date(Date.now())
           }
         })
         return messages;
      })
    }
   })
}   

stopHubConnection () {
  if (this.hubConnection?.state === HubConnectionState.Connected)
    this.hubConnection.stop().catch(error => console.log(error) );
}

  getMessages(pageNumber : number , pageSize : number , container : string) {
    let params = setPaginationHeaders(pageNumber , pageSize);

    params = params.append('Container' , container);

    return this.http.get<Message[]>(this.baseUrl + 'messages', {observe : 'response' , params})
    .subscribe({
      next : response =>{
       console.log(response);
        setPaginatedResponse(response , this.paginatedResult)

      } 
    })
  }

  getMessageThread (username: string)  {
   return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  async sendMessage(username : string , content :string)  {

    // return this.http.post<Message>(this.baseUrl + 'messages' , {recipientUserName:username , content });
     
    return this.hubConnection?.invoke('SendMessage' , {recipientUsername:username , content})

  }

  deleteMessage( id: number) {

    return this.http.delete(this.baseUrl + 'messages/' + id);

  }
  
}
