pipeline {
  agent any
  stages {
    stage('Hello') {
      steps {
        echo 'Hello jenkins'
      }
    }
    stage('Build Docker Image') {
      steps {
        sh 'cd 1_Calculator/1_Calculator && ls -l'
        sh 'cd 1_Calculator/1_Calculator && docker compose build'
      }
    }
    stage('Start Docker Container') {
      steps {
        sh 'cd 1_Calculator/1_Calculator && docker compose up -d'
      }
    }
  }
}
