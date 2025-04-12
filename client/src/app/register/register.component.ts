import { Component, inject,Output, EventEmitter, OnInit, } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, RequiredValidator, ValidatorFn, Validators } from '@angular/forms';
import { AccountsService } from '../_services/accounts.service';
import { ToastrService } from 'ngx-toastr';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule,NgIf],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent  implements OnInit{
  private accountService = inject(AccountsService)
  private fb = inject(FormBuilder);
  private toastr = inject(ToastrService)
 model : any = {};
 @Output() cancelRegister = new EventEmitter<boolean>();
 registerForm : FormGroup = new FormGroup({});

 ngOnInit(): void {
   this.initializeForm();
 } 

 initializeForm() {
    this.registerForm  = this.fb.group({
      gender :['male'],
      knownAs : ['' , Validators.required],
      dateOfBirth :  ['' , Validators.required],
      city :  ['' , Validators.required],
      country :  ['' , Validators.required],
      username : ['' , [Validators.required]],
      password : ['',[Validators.required , Validators.minLength(4),Validators.maxLength(8)]],
      confirmPassword : ['',[Validators.required , this.matchValues('password')]]
    });

    this.registerForm.controls['password'].valueChanges.subscribe({
      next :() => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })
 }

 matchValues(matchTo : string):ValidatorFn{
   return (control:AbstractControl) => {
    return control.value === control.parent?.get(matchTo)?.value ? null : {isMatching:true}
   }
 }
 
 cancel() {
  this.cancelRegister.emit(false)
}

  register() {
  this.accountService.register(this.model).subscribe({
    next : response => {
      console.log(response);
      this.cancel();
    },
    error : error => this.toastr.error(error.error)
   
  })
  }



}
