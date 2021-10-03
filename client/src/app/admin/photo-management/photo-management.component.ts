import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/models/photo';
import { AdminService } from 'src/app/_services/admin-service.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {

  constructor(private adminService: AdminService) { }

  photos: Photo[] = []

  ngOnInit(): void {
    this.getPhotosForApproval()
  }


  getPhotosForApproval(){
    this.adminService.getPhotosForApproval().subscribe(result => {
      this.photos = result
    }, e => console.log(e))
  }

  approvePhoto(photo: Photo){
    this.adminService.approvePhoto(photo.id).subscribe(result => {
      let index = this.photos.indexOf(photo)
      if (index > -1) {
        this.photos.splice(index, 1);
      }
    }, e => console.log(e))
  }

  rejectPhoto(photo: Photo){
    this.adminService.rejectPhoto(photo.id).subscribe(result => {
      let index = this.photos.indexOf(photo)
      if (index > -1) {
        this.photos.splice(index, 1);
      }
    }, e => console.log(e))
  }
}
