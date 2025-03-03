import { ChangeDetectionStrategy, Component, input, OnInit, SecurityContext, signal } from '@angular/core';
import { QuestionsComponent } from '../questions/questions.component';
import { ButtonComponent } from '../shared/button/button.component';
import { HttpClient, HttpParams } from '@angular/common/http';
import { finalize } from 'rxjs';
import { ToastService } from '../toast.service';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-job-detail',
  imports: [
    ButtonComponent,
    QuestionsComponent,
  ],
  templateUrl: './jobDetail.component.html',
  styleUrl: './jobDetail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JobDetailComponent implements OnInit {
  jobID = input.required<number>();
  safeHtml = signal<string | null>(null);

  constructor(private http: HttpClient, private toastService: ToastService, private sanitizer: DomSanitizer) {}

  ngOnInit(): void {
    let params = new HttpParams().set('jobID', this.jobID());

    const options: Object = {
      params: params,
      responseType: 'text',
    }
    
    this.http.get<string | null>('api/JobDetails', options)
      .subscribe({
        next: (html) => {
          const safe = this.sanitizer.sanitize(SecurityContext.HTML, html ?? "");
          this.safeHtml.set(safe);
        }
      });
  }

  submitting = signal<boolean>(false);

  clicked(): void {
      this.submitting.set(true);
  
      let params = new HttpParams()
        .set('jobID', this.jobID());
  
      this.http.get('api/apply', { params: params })
        .pipe(finalize(() => this.submitting.set(false)))
        .subscribe({
          next: (x) => {
            this.toastService.show('Success');
          },
          error: () => {
            this.toastService.show('Error');
          },
        });
    }
}
