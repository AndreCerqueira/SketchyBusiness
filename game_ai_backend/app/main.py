from fastapi import FastAPI
from pydantic import BaseModel
import ollama

app = FastAPI(title="Teste de Ligacao")

class ImageRequest(BaseModel):
    ImageBase64: str
    Topic: str

@app.get("/hello-test")
def hello_test():
    return {"response": "Hello World do Python! A ponte esta a funcionar!"}

@app.post("/analyze-drawing")
def analyze_drawing(request: ImageRequest):
    prompt = (
        f"Atua como um jogador num jogo de adivinhas tipo Pictionary. "
        f"A imagem é apenas um simples rabisco ou esboço feito com traços pretos num fundo branco. "
        f"O tema do desenho é '{request.Topic}'. "
        f"Regras: "
        f"1. Não menciones cores, tipos de linha, fundos ou o facto de ser um desenho. "
        f"2. Foca-te puramente em adivinhar qual é a coisa dentro do tema '{request.Topic}' que aquelas linhas formam. "
        f"3. Responde de forma muito curta e direta, em inglês. "
        f"Diz-me apenas o que achas que o jogador tentou desenhar."
    )

    try:
        print("A processar a imagem com o LLaVA. Aguarda...")

        resposta = ollama.chat(
            model='llava', 
            messages=[
                {
                    'role': 'user',
                    'content': prompt,
                    'images': [request.ImageBase64]
                }
            ]
        )
        
        descricao = resposta['message']['content']
        print(f"Resposta do LLaVA: {descricao}")
        return {"Description": descricao}
        
    except Exception as e:
        return {"Description": f"Ocorreu um erro a analisar o desenho: {str(e)}"}