FROM node
WORKDIR /usr/src/app
COPY package*.json ./
RUN npm i
COPY . .
EXPOSE 4000, 3001
CMD ["npm", "start"]