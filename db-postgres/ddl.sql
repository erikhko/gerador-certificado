CREATE DATABASE diploma_db;

CREATE TABLE diplomas (
    id SERIAL PRIMARY KEY,
    nome_aluno VARCHAR(255),
    nome_curso VARCHAR(255),
    data_conclusao DATE,
    data_nascimento DATE,
    data_emissao TIMESTAMP,
    nacionalidade VARCHAR(255),
    naturalidade VARCHAR(255),
    numero_rg VARCHAR(20)
);