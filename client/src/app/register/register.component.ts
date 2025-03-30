import { Component, inject,Output, EventEmitter, } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountsService } from '../_services/accounts.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountsService)
 model : any = {};
 @Output() cancelRegister = new EventEmitter<boolean>();

 
 cancel() {
  this.cancelRegister.emit(false)
}

  register() {
  this.accountService.register(this.model).subscribe({
    next : response => {
      console.log(response);
      this.cancel();
    }
   
  })
  }



}
