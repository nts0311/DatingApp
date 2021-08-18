import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import {LoginModel} from '../models/LoginModel';
import { User } from '../models/User';
import { AccountService } from '../_services/account.service';


@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model : LoginModel = new LoginModel()

  constructor(public accountService:AccountService) { }

  ngOnInit(): void {
  }

  login()
  {
    this.accountService.login(this.model).subscribe(response => {
      
      console.log(response)
      
    }, error => console.log(error))
  }

  logout(){
    this.accountService.logout()
  }
}
