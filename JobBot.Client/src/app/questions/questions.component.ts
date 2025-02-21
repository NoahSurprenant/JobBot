import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { ToastService } from '../toast.service';
import { finalize } from 'rxjs';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'app-questions',
  imports: [
    ButtonComponent,
  ],
  templateUrl: './questions.component.html',
  styleUrl: './questions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuestionsComponent {
  loading = signal<boolean>(false);
  questions = signal<QuestionDto[]>([]);

  constructor(private http: HttpClient, private toastService: ToastService) {}

  clicked(): void {
    this.loading.set(true);

    this.http.get<QuestionDto[]>('api/questions')
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (x) => {
          this.questions.set(x);
          this.toastService.show('Success');
        },
        error: () => {
          this.toastService.show('Error');
        },
      })
  }
}

export interface QuestionDto
{
  label: string,
  value: string | null,
  questionPage: 'ContactInfo' | 'AdditionalQuestions' | 'WorkAuthorization',
  questionKind: 'AutoLine' | 'ComboBox' | 'Radio' | 'SingleLine',
  options: string[] | null,
}