import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Pagination, PaginationedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  paginatedResult = signal<PaginationedResult<Message[]>|null>(null)

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

  sendMessage(username : string , content :string)  {

    return this.http.post<Message>(this.baseUrl + 'messages' , {recipientUserName:username , content });

  }

  deleteMessage( id: number) {

    return this.http.delete(this.baseUrl + 'messages/' + id);

  }
  
}
