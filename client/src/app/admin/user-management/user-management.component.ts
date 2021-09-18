import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/models/User';
import { AdminService } from 'src/app/_services/admin-service.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]>
  bsModalRef: BsModalRef

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles()
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRole().subscribe(users => {
      this.users = users
    })
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, config)
    this.bsModalRef.content.updateSelectedRoles.subscribe(values => {
      const roles = values.filter(el => el.checked).map(el => el.name)
      if(roles){
        this.adminService.updateUserRoles(user.username, roles).subscribe(() => {
          user.roles = roles
        })
      }
    }) 
  }

  private getRolesArray(user: User){
    const roles = []
    const userRoles = user.roles 

    const availableRoles = [
      {name: 'Admin', value: 'Admin', checked: false},
      {name: 'Moderator', value: 'Moderator', checked: false},
      {name: 'Member', value: 'Member', checked: false}
    ]

    availableRoles.forEach(role => {
      for(let userRole of userRoles){
        if(userRole === role.value){
          role.checked = true
          break
        }
      }
      roles.push(role)
    })

    return roles
  }


}
