import { Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
import { Message } from '../../_models/message';
import { MessagesService } from '../../_services/messages.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule , FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm? : NgForm;
  username  = input.required<string>();
  messages = input.required<Message[]>();
  private messageService = inject(MessagesService);
  messageContent = '';
  updateMessages = output<Message>();

  ngOnInit(): void {
    
  }

  sendMessage() {
    this.messageService.sendMessage(this.username() , this.messageContent).subscribe({
      next: message => {
        this.updateMessages.emit(message)
        this.messageForm?.reset();
      }
    })
  }



  // loadMessages() {
  //   this.messageService.getMessageThread(this.username()).subscribe({
  //     next :
  //      messages =>   this.messages = messages
       
  //   })
  // }
}
