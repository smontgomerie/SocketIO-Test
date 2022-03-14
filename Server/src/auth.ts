import fs from "fs";
import {log} from "./log";
import {User} from "./user";

const passport = require('passport');
const JwtStrategy = require('passport-jwt').Strategy;
const ExtractJwt = require('passport-jwt').ExtractJwt;

const users = {};

//#region Passport strategies by environment
//load cms secret from file / environment variable directly
let cmsSecret;
if (process.env.CMS_JWT_SECRET_FILE) {
  try {
    cmsSecret = fs.readFileSync(process.env.CMS_JWT_SECRET_FILE, 'utf8').replace('\n', '');
  }
  catch (e) {
    log.warn(e, 'Failed to load the specified CMS_JWT_SECRET_FILE, fallback - CMS_JWT_SECRET property');
    cmsSecret = process.env.CMS_JWT_SECRET;
  }
}
else {
  cmsSecret = process.env.CMS_JWT_SECRET;
}
const opts = {
  jwtFromRequest: ExtractJwt.fromExtractors([ExtractJwt.fromAuthHeaderAsBearerToken(),
    ExtractJwt.fromUrlQueryParameter('jwt')]), //fallback to cookie if header isn't present
  secretOrKey: cmsSecret,
  passReqToCallback: true
}

export const validateToken = (authToken) => {
  // ... validate token and return a Promise, rejects in case of an error
  return new Promise<boolean>((resolve, reject) => {
    resolve(true);
    return true;
  });
}

export const findUser = (authToken) => {
  return (tokenValidationResult) => {
    // ... finds user by auth token and return a Promise, rejects in case of an error
    return new Promise<User>((resolve, reject) => {
      resolve(users[authToken] ||= {
        id: authToken,
        description: "",
        data: ""
      });
    });
  }
}