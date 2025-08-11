CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    password TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS certificates (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(id),
    title TEXT NOT NULL,
    image_url TEXT NOT NULL
);

INSERT INTO users (name, email, password)
VALUES ('Raynara Martins', 'ray@teste.com', '123456')
ON CONFLICT (email) DO NOTHING;


INSERT INTO certificates (user_id, title, image_url) VALUES
  (1, 'Certificado exemplo API com Postman', 'https://via.placeholder.com/150?text=Postman'),
  (1, 'Certificado exemplo Docker Básico',            'https://via.placeholder.com/150?text=Docker'),
  (1, 'Certificado exemplo Testes Automatizados',     'https://via.placeholder.com/150?text=NUnit'),
  (1, 'Certificado exemplo UI/UX Design',             'https://via.placeholder.com/150?text=UX');
