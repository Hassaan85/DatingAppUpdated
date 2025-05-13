import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpTransportType, HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';
import { take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
hubUrl = environment.hubsUrl;
private hubConnection? : HubConnection;
private toastr = inject(ToastrService);
private router = inject(Router)
onlineUsers = signal<string[]>([]);


createHubConnection(user : User) {
  this.hubConnection = new HubConnectionBuilder()
       .withUrl(this.hubUrl +  'presence', {
        accessTokenFactory : () =>user.token,
        transport: HttpTransportType.WebSockets
       })
       .withAutomaticReconnect()
       .build();

  this.hubConnection.start().catch(error => console.log(error));

  this.hubConnection.on('User is online' , username => {
    // this.toastr.info(username + ' has connected')
    this.onlineUsers.update(users => [...users , username]);
});

  this.hubConnection.on('User is offline' , username => {
    // this.toastr.warning(username + ' has disconnected')
    this.onlineUsers.update(users => users.filter(x => x !== username));
});

  this.hubConnection.on('Get Online Users', usernames => {
    this.onlineUsers.set(usernames);
  })

  this.hubConnection.on('New Message Received' , ({username , knownAs}) => {
    this.toastr.info(knownAs + 'Has sent you a new message! Click me to see it')
    .onTap
    .pipe(take(1))
    .subscribe(()=> this.router.navigateByUrl('/members/' + username + '?tab=Messages'))

  })
}

stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected)
      this.hubConnection.stop();
}
  
}
