from fastapi import FastAPI
from pydantic import BaseModel
import ollama

app = FastAPI(title="Teste de Ligacao")

class ImageRequest(BaseModel):
    ImageBase64: str

@app.get("/hello-test")
def hello_test():
    return {"response": "Hello World do Python! A ponte esta a funcionar!"}

@app.post("/analyze-drawing")
def analyze_drawing(request: ImageRequest):
    print("Recebi um pedido do Unity!")
    print(f"Tamanho da imagem recebida: {len(request.ImageBase64)} caracteres")
    
    try:
        # Pede ao Ollama (modelo LLaVA) para analisar a imagem
        print("A processar a imagem com o LLaVA. Aguarda...")
        
        resposta = ollama.chat(
            model='llava', 
            messages=[
                {
                    'role': 'user',
                    # Uma boa instrução (prompt) ajuda a IA a perceber que é um desenho de linhas
                    'content': 'Esta imagem é um desenho feito à mão. Descreve o que achas que está desenhado aqui de forma curta e direta.',
                    'images': [request.ImageBase64]
                }
            ]
        )
        
        # Extrai o texto da resposta
        descricao = resposta['message']['content']
        print(f"Resposta do LLaVA: {descricao}")
        
        return {"Description": descricao}
        
    except Exception as e:
        erro = str(e)
        print("Erro ao processar com LLaVA:", erro)
        return {"Description": f"Ocorreu um erro a analisar o desenho: {erro}"}