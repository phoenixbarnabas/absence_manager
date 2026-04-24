import { platformBrowser } from '@angular/platform-browser';
import { AppModule } from './app/app-module';

async function bootstrap(): Promise<void> {
  try {
    await platformBrowser().bootstrapModule(AppModule);
  } catch (err) {
    console.error(err);
  }
}

bootstrap();