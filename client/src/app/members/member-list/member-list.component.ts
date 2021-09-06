import { Component, OnInit } from '@angular/core';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/models/member';
import { Pagination } from 'src/app/models/pagination';
import { User } from 'src/app/models/User';
import { UserParams } from 'src/app/models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[]
  pagination: Pagination
  userParams: UserParams
  user: User
  genderList = [{value: 'male', display: 'Males'}, {value: 'female', display: 'Females'}]

  constructor(private memberService: MembersService) {
    this.userParams = memberService.getUserParams()
  }

  ngOnInit(): void {
    this.loadMembers()
  }

  loadMembers(){
    //this.memberService.setUserParams(this.userParams)
    this.memberService.getMembers(this.userParams).subscribe(response => {
      this.members = response.result
      this.pagination = response.pagination
    })
  }

  pageChanged(event: PageChangedEvent){
    this.userParams.pageNumber = event.page
    //this.memberService.setUserParams(this.userParams)
    this.loadMembers()
  }

  resetFilter(){
    this.userParams = this.memberService.resetUserParams()
    this.loadMembers()
  }
}
