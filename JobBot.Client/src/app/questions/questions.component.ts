import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { ToastService } from '../toast.service';
import { finalize } from 'rxjs';
import { ButtonComponent } from '../button/button.component';
import { InputComponent } from '../input/input.component';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { QuestionBase, QuestionControlService } from '../questionControl.service';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-questions',
  imports: [
    CommonModule,
    ButtonComponent,
    InputComponent,
    DropdownComponent,
    FormsModule,
    ReactiveFormsModule,
  ],
  templateUrl: './questions.component.html',
  styleUrl: './questions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuestionsComponent {
  loading = signal<boolean>(false);
  questions = signal<QuestionDto[]>([]);
  form!: FormGroup;

  constructor(private http: HttpClient, private toastService: ToastService, private qcs: QuestionControlService) {}

  isEven(i: number) {
    return i % 2 == 0;
  }

  isOdd(i: number) {
    return !this.isEven(i);
  }

  clicked(): void {
    this.loading.set(true);

    this.http.get<QuestionDto[]>('api/questions')
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (x) => {
          this.questions.set(x);
          this.form = this.qcs.toFormGroup(this.toQuestionBase(x));
          this.toastService.show('Success');
        },
        error: () => {
          this.toastService.show('Error');
        },
      })
  }

  toQuestionBase(x: QuestionDto[]): QuestionBase<string>[] {
    return x.map(t => new QuestionBase<string>({
      value: t.value ?? undefined,
      key: t.label,//.replace(".", "_"),
      //label: t.label,
      //required: false,
      //controlType: (t.questionKind == 'SingleLine' || t.questionKind == 'AutoLine') ? 'textbox' : (t.questionKind == 'ComboBox' || t.questionKind == 'Radio') ? 'dropdown' : ''
    }));
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
